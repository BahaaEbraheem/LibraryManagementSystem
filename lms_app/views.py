


from django.shortcuts import render
from .forms import bookForm,CategoryForm
from .models import Book, Category


# Create your views here.
def index(request):
     if request.method == 'POST':
        add_book = bookForm(request.POST, request.FILES)
        if add_book.is_valid():
           add_book.save()
        else:
              print("❌ أخطاء في نموذج الكتاب:", add_book.errors)

        add_category = CategoryForm(request.POST, request.FILES)
        if add_category.is_valid():
           add_category.save()
        else:
              print("❌ أخطاء في نموذج الكتاب:", add_category.errors)
     context = {   
          'books':Book.objects.all(),
          'categories':Category.objects.all(),
          'form' : bookForm(),
          'categoryForm' : CategoryForm(),
          }
     
     return render(request,'pages/index.html',context)

def books(request):
     context = {   
          'books':Book.objects.all(),
          'categories':Category.objects.all(),
          }
     
     return render(request,'pages/books.html',context)

def update(request):
    return render(request,'pages/update.html')

def delete(request):
    return render(request,'pages/delete.html') ; 
